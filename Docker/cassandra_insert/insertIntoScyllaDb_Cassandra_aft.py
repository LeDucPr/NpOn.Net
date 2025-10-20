import os
from cassandra.cluster import Cluster
from cassandra.query import BatchStatement, ConsistencyLevel
from cassandra import InvalidRequest
scylla_host = "127.0.0.1"
port = 9042
cluster = Cluster([scylla_host], port=port)
session = cluster.connect()
session.set_keyspace("scarlight")  # Thay bằng tên keyspace của bạn
COLUMNS = [
    "actype", "typename", "aftype", "ifrulecd", "approvec", "floorlimit", "branchlimit", "telelimit", "onlinelimit",
    "marginline", "repoline", "advancedline", "status", "linetied", "citype", "setype", "description", "iccfcd",
    "iccftied", "bratio", "depositline", "traderate", "deporate", "miscrate", "stmcycle", "minbal", "dormday",
    "tieddelta", "isotc", "consultant", "tiedfeebase", "tieddebate", "tlmodcode", "corebank", "mrtype", "lntype",
    "dftype", "afgrp", "custodiantyp", "pstatus", "apprv_sts", "adtype", "autoadv", "policycd", "debitifdebt",
    "t0lntype", "autodf", "mnemonic", "trfbuyext", "istrfbuy", "k1days", "k2days", "advprio", "chkmarginbuy",
    "producttype", "mrcrlimitmax", "glgrptype", "opnonline"
]
NUM_COLUMNS = len(COLUMNS)
def parse_sql_file(filename):
    import re
    with open(filename, encoding="utf-8") as f:
        content = f.read()
    pattern = re.compile(r"VALUES\s*\((.*?)\);", re.DOTALL | re.IGNORECASE)
    matches = pattern.findall(content)
    rows = []
    for idx, match in enumerate(matches):
        values = []
        current = ''
        in_string = False
        for c in match:
            if c == "'":
                in_string = not in_string
                current += c
            elif c == "," and not in_string:
                values.append(current.strip())
                current = ''
            else:
                current += c
        if current:
            values.append(current.strip())
        if len(values) != NUM_COLUMNS:
            print(f"[WARNING] Bỏ qua dòng {idx+1} vì số lượng giá trị ({len(values)}) không khớp số cột ({NUM_COLUMNS})")
            continue
        values = [None if v.upper() == "NULL" else v.strip("'") for v in values]
        rows.append(values)
    for row in rows:
        row[COLUMNS.index("floorlimit")] = float(row[COLUMNS.index("floorlimit")]) if row[COLUMNS.index("floorlimit")] else None
        row[COLUMNS.index("branchlimit")] = float(row[COLUMNS.index("branchlimit")]) if row[COLUMNS.index("branchlimit")] else None
        row[COLUMNS.index("telelimit")] = float(row[COLUMNS.index("telelimit")]) if row[COLUMNS.index("telelimit")] else None
        row[COLUMNS.index("onlinelimit")] = float(row[COLUMNS.index("onlinelimit")]) if row[COLUMNS.index("onlinelimit")] else None
        row[COLUMNS.index("marginline")] = float(row[COLUMNS.index("marginline")]) if row[COLUMNS.index("marginline")] else None
        row[COLUMNS.index("repoline")] = float(row[COLUMNS.index("repoline")]) if row[COLUMNS.index("repoline")] else None
        row[COLUMNS.index("advancedline")] = float(row[COLUMNS.index("advancedline")]) if row[COLUMNS.index("advancedline")] else None
        row[COLUMNS.index("bratio")] = float(row[COLUMNS.index("bratio")]) if row[COLUMNS.index("bratio")] else None
        row[COLUMNS.index("depositline")] = float(row[COLUMNS.index("depositline")]) if row[COLUMNS.index("depositline")] else None
        row[COLUMNS.index("traderate")] = float(row[COLUMNS.index("traderate")]) if row[COLUMNS.index("traderate")] else None
        row[COLUMNS.index("deporate")] = float(row[COLUMNS.index("deporate")]) if row[COLUMNS.index("deporate")] else None
        row[COLUMNS.index("miscrate")] = float(row[COLUMNS.index("miscrate")]) if row[COLUMNS.index("miscrate")] else None
        row[COLUMNS.index("minbal")] = float(row[COLUMNS.index("minbal")]) if row[COLUMNS.index("minbal")] else None
        row[COLUMNS.index("dormday")] = int(row[COLUMNS.index("dormday")]) if row[COLUMNS.index("dormday")] else None
        row[COLUMNS.index("tieddelta")] = float(row[COLUMNS.index("tieddelta")]) if row[COLUMNS.index("tieddelta")] else None
        row[COLUMNS.index("tiedfeebase")] = float(row[COLUMNS.index("tiedfeebase")]) if row[COLUMNS.index("tiedfeebase")] else None
        row[COLUMNS.index("tieddebate")] = float(row[COLUMNS.index("tieddebate")]) if row[COLUMNS.index("tieddebate")] else None
        row[COLUMNS.index("mrcrlimitmax")] = float(row[COLUMNS.index("mrcrlimitmax")]) if row[COLUMNS.index("mrcrlimitmax")] else None
        row[COLUMNS.index("k1days")] = int(row[COLUMNS.index("k1days")]) if row[COLUMNS.index("k1days")] else None
        row[COLUMNS.index("k2days")] = int(row[COLUMNS.index("k2days")]) if row[COLUMNS.index("k2days")] else None
        row[COLUMNS.index("trfbuyext")] = float(row[COLUMNS.index("trfbuyext")]) if row[COLUMNS.index("trfbuyext")] else None
    return rows
def write_error_log(row, index, error_msg):
    with open("insert_AFTYPE_error.log", "a", encoding="utf-8") as f:
        f.write(f"[Row {index}] ERROR: {error_msg}\n{row}\n\n")
def insert_batch_recursive(prepared, rows, indices):
    try:
        batch = BatchStatement(consistency_level=ConsistencyLevel.QUORUM)
        for row in rows:
            batch.add(prepared, row)
        session.execute(batch)
        print(f"✅ Inserted rows: {indices}")
    except Exception as e:
        if len(rows) == 1:
            idx = indices[0]
            write_error_log(rows[0], idx, str(e))
            print(f"❌ Failed to insert row {idx}: {e}")
        else:
            mid = len(rows) // 2
            insert_batch_recursive(prepared, rows[:mid], indices[:mid])
            insert_batch_recursive(prepared, rows[mid:], indices[mid:])
def bulk_insert(rows, batch_size=100):
    insert_stmt = f"""
    INSERT INTO aftype (
        {', '.join(COLUMNS)}
    ) VALUES ({', '.join(['?' for _ in range(NUM_COLUMNS)])})
    """
    prepared = session.prepare(insert_stmt)
    for i in range(0, len(rows), batch_size):
        batch_rows = rows[i:i + batch_size]
        indices = list(range(i + 1, i + 1 + len(batch_rows)))  # Dòng bắt đầu từ 1
        insert_batch_recursive(prepared, batch_rows, indices)
    print("⚡ Bulk insert finished. Check insert_AFTYPE_error.log for any failed rows.")
if __name__ == "__main__":
    if os.path.exists("insert_AFTYPE_error.log"):
        os.remove("insert_AFTYPE_error.log")
    rows = parse_sql_file("aft.sql")
    bulk_insert(rows)