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
    "actype", "acctno", "ccycd", "afacctno", "custid", "opndate", "clsdate", "lastdate", "dormdate", "status",
    "pstatus", "balance", "cramt", "dramt", "crintacr", "crintdt", "odintacr", "odintdt", "avrbal", "mdebit",
    "mcredit", "aamt", "ramt", "bamt", "emkamt", "mmarginbal", "marginbal", "iccfcd", "iccftied", "odlimit",
    "adintacr", "adintdt", "facrtrade", "facrdepository", "facrmisc", "minbal", "odamt", "namt", "floatamt",
    "holdbalance", "pendinghold", "pendingunhold", "corebank", "receiving", "netting", "mblock", "ovamt", "dueamt",
    "t0odamt", "mbalance", "mcrintdt", "trfamt", "last_change", "dfodamt", "dfdebtamt", "dfintdebtamt",
    "cidepofeeacr", "trfbuyamt", "intfloatamt", "feefloatamt", "depolastdt", "depofeeamt", "holdmnlamt", "t0ovdamt",
    "bankbalance", "bankavlbal", "bankinqirydt", "intbuyamt", "intcaamt", "buysecamt", "intmrnrate"
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
        row[COLUMNS.index("balance")] = float(row[COLUMNS.index("balance")]) if row[COLUMNS.index("balance")] else None
        row[COLUMNS.index("cramt")] = float(row[COLUMNS.index("cramt")]) if row[COLUMNS.index("cramt")] else None
        row[COLUMNS.index("dramt")] = float(row[COLUMNS.index("dramt")]) if row[COLUMNS.index("dramt")] else None
        row[COLUMNS.index("crintacr")] = float(row[COLUMNS.index("crintacr")]) if row[COLUMNS.index("crintacr")] else None
        row[COLUMNS.index("odintacr")] = float(row[COLUMNS.index("odintacr")]) if row[COLUMNS.index("odintacr")] else None
        row[COLUMNS.index("avrbal")] = float(row[COLUMNS.index("avrbal")]) if row[COLUMNS.index("avrbal")] else None
        row[COLUMNS.index("mdebit")] = float(row[COLUMNS.index("mdebit")]) if row[COLUMNS.index("mdebit")] else None
        row[COLUMNS.index("mcredit")] = float(row[COLUMNS.index("mcredit")]) if row[COLUMNS.index("mcredit")] else None
        row[COLUMNS.index("aamt")] = float(row[COLUMNS.index("aamt")]) if row[COLUMNS.index("aamt")] else None
        row[COLUMNS.index("ramt")] = float(row[COLUMNS.index("ramt")]) if row[COLUMNS.index("ramt")] else None
        row[COLUMNS.index("bamt")] = float(row[COLUMNS.index("bamt")]) if row[COLUMNS.index("bamt")] else None
        row[COLUMNS.index("emkamt")] = float(row[COLUMNS.index("emkamt")]) if row[COLUMNS.index("emkamt")] else None
        row[COLUMNS.index("mmarginbal")] = float(row[COLUMNS.index("mmarginbal")]) if row[COLUMNS.index("mmarginbal")] else None
        row[COLUMNS.index("marginbal")] = float(row[COLUMNS.index("marginbal")]) if row[COLUMNS.index("marginbal")] else None
        row[COLUMNS.index("odlimit")] = float(row[COLUMNS.index("odlimit")]) if row[COLUMNS.index("odlimit")] else None
        row[COLUMNS.index("adintacr")] = float(row[COLUMNS.index("adintacr")]) if row[COLUMNS.index("adintacr")] else None
        row[COLUMNS.index("facrtrade")] = float(row[COLUMNS.index("facrtrade")]) if row[COLUMNS.index("facrtrade")] else None
        row[COLUMNS.index("facrdepository")] = float(row[COLUMNS.index("facrdepository")]) if row[COLUMNS.index("facrdepository")] else None
        row[COLUMNS.index("facrmisc")] = float(row[COLUMNS.index("facrmisc")]) if row[COLUMNS.index("facrmisc")] else None
        row[COLUMNS.index("minbal")] = float(row[COLUMNS.index("minbal")]) if row[COLUMNS.index("minbal")] else None
        row[COLUMNS.index("odamt")] = float(row[COLUMNS.index("odamt")]) if row[COLUMNS.index("odamt")] else None
        row[COLUMNS.index("namt")] = float(row[COLUMNS.index("namt")]) if row[COLUMNS.index("namt")] else None
        row[COLUMNS.index("floatamt")] = float(row[COLUMNS.index("floatamt")]) if row[COLUMNS.index("floatamt")] else None
        row[COLUMNS.index("holdbalance")] = float(row[COLUMNS.index("holdbalance")]) if row[COLUMNS.index("holdbalance")] else None
        row[COLUMNS.index("pendinghold")] = float(row[COLUMNS.index("pendinghold")]) if row[COLUMNS.index("pendinghold")] else None
        row[COLUMNS.index("pendingunhold")] = float(row[COLUMNS.index("pendingunhold")]) if row[COLUMNS.index("pendingunhold")] else None
        row[COLUMNS.index("mblock")] = float(row[COLUMNS.index("mblock")]) if row[COLUMNS.index("mblock")] else None
        row[COLUMNS.index("ovamt")] = float(row[COLUMNS.index("ovamt")]) if row[COLUMNS.index("ovamt")] else None
        row[COLUMNS.index("dueamt")] = float(row[COLUMNS.index("dueamt")]) if row[COLUMNS.index("dueamt")] else None
        row[COLUMNS.index("t0odamt")] = float(row[COLUMNS.index("t0odamt")]) if row[COLUMNS.index("t0odamt")] else None
        row[COLUMNS.index("mbalance")] = float(row[COLUMNS.index("mbalance")]) if row[COLUMNS.index("mbalance")] else None
        row[COLUMNS.index("trfamt")] = float(row[COLUMNS.index("trfamt")]) if row[COLUMNS.index("trfamt")] else None
        row[COLUMNS.index("dfodamt")] = float(row[COLUMNS.index("dfodamt")]) if row[COLUMNS.index("dfodamt")] else None
        row[COLUMNS.index("dfdebtamt")] = float(row[COLUMNS.index("dfdebtamt")]) if row[COLUMNS.index("dfdebtamt")] else None
        row[COLUMNS.index("dfintdebtamt")] = float(row[COLUMNS.index("dfintdebtamt")]) if row[COLUMNS.index("dfintdebtamt")] else None
        row[COLUMNS.index("cidepofeeacr")] = float(row[COLUMNS.index("cidepofeeacr")]) if row[COLUMNS.index("cidepofeeacr")] else None
        row[COLUMNS.index("trfbuyamt")] = float(row[COLUMNS.index("trfbuyamt")]) if row[COLUMNS.index("trfbuyamt")] else None
        row[COLUMNS.index("intfloatamt")] = float(row[COLUMNS.index("intfloatamt")]) if row[COLUMNS.index("intfloatamt")] else None
        row[COLUMNS.index("feefloatamt")] = float(row[COLUMNS.index("feefloatamt")]) if row[COLUMNS.index("feefloatamt")] else None
        row[COLUMNS.index("depofeeamt")] = float(row[COLUMNS.index("depofeeamt")]) if row[COLUMNS.index("depofeeamt")] else None
        row[COLUMNS.index("holdmnlamt")] = float(row[COLUMNS.index("holdmnlamt")]) if row[COLUMNS.index("holdmnlamt")] else None
        row[COLUMNS.index("t0ovdamt")] = float(row[COLUMNS.index("t0ovdamt")]) if row[COLUMNS.index("t0ovdamt")] else None
        row[COLUMNS.index("bankbalance")] = float(row[COLUMNS.index("bankbalance")]) if row[COLUMNS.index("bankbalance")] else None
        row[COLUMNS.index("bankavlbal")] = float(row[COLUMNS.index("bankavlbal")]) if row[COLUMNS.index("bankavlbal")] else None
        row[COLUMNS.index("intmrnrate")] = float(row[COLUMNS.index("intmrnrate")]) if row[COLUMNS.index("intmrnrate")] else None
    return rows
def write_error_log(row, index, error_msg):
    with open("insert_CIMAST_error.log", "a", encoding="utf-8") as f:
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
    INSERT INTO cimast (
        {', '.join(COLUMNS)}
    ) VALUES ({', '.join(['?' for _ in range(NUM_COLUMNS)])})
    """
    prepared = session.prepare(insert_stmt)
    for i in range(0, len(rows), batch_size):
        batch_rows = rows[i:i + batch_size]
        indices = list(range(i + 1, i + 1 + len(batch_rows)))  # Dòng bắt đầu từ 1
        insert_batch_recursive(prepared, batch_rows, indices)
    print("⚡ Bulk insert finished. Check insert_CIMAST_error.log for any failed rows.")
if __name__ == "__main__":
    if os.path.exists("insert_CIMAST_error.log"):
        os.remove("insert_CIMAST_error.log")
    rows = parse_sql_file("cim.sql")
    bulk_insert(rows)