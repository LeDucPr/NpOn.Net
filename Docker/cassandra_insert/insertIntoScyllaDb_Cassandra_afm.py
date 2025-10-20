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
    "actype", "custid", "acctno", "aftype", "bankacctno", "bankname", "swiftcode", "lastdate", "status", "pstatus",
    "advanceline", "depositline", "bratio", "termofuse", "description", "isotc", "pisotc", "opndate", "corebank",
    "via", "mrirate", "mrmrate", "mrlrate", "mrcrlimit", "mrcrlimitmax", "groupleader", "t0amt", "brid", "last_change",
    "clsdate", "careby", "autoadv", "tlid", "mriratio", "mrmratio", "mrlratio", "depolastdt", "brkfeetype", "triggerdate",
    "alternateacct", "callday", "limitdaily", "isfixaccount", "autotrf", "chgactype", "mrcrate", "mrwrate", "k1days",
    "k2days", "mrexrate", "producttype", "iscieod", "ispm", "isdebtt0", "tradeline", "tradebl", "clamtlimit", "chstatus",
    "tradeonline"
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
        row[COLUMNS.index("advanceline")] = float(row[COLUMNS.index("advanceline")]) if row[COLUMNS.index("advanceline")] else None
        row[COLUMNS.index("depositline")] = float(row[COLUMNS.index("depositline")]) if row[COLUMNS.index("depositline")] else None
        row[COLUMNS.index("bratio")] = float(row[COLUMNS.index("bratio")]) if row[COLUMNS.index("bratio")] else None
        row[COLUMNS.index("mrirate")] = float(row[COLUMNS.index("mrirate")]) if row[COLUMNS.index("mrirate")] else None
        row[COLUMNS.index("mrmrate")] = float(row[COLUMNS.index("mrmrate")]) if row[COLUMNS.index("mrmrate")] else None
        row[COLUMNS.index("mrlrate")] = float(row[COLUMNS.index("mrlrate")]) if row[COLUMNS.index("mrlrate")] else None
        row[COLUMNS.index("mrcrlimit")] = float(row[COLUMNS.index("mrcrlimit")]) if row[COLUMNS.index("mrcrlimit")] else None
        row[COLUMNS.index("mrcrlimitmax")] = float(row[COLUMNS.index("mrcrlimitmax")]) if row[COLUMNS.index("mrcrlimitmax")] else None
        row[COLUMNS.index("t0amt")] = float(row[COLUMNS.index("t0amt")]) if row[COLUMNS.index("t0amt")] else None
        row[COLUMNS.index("mriratio")] = float(row[COLUMNS.index("mriratio")]) if row[COLUMNS.index("mriratio")] else None
        row[COLUMNS.index("mrmratio")] = float(row[COLUMNS.index("mrmratio")]) if row[COLUMNS.index("mrmratio")] else None
        row[COLUMNS.index("mrlratio")] = float(row[COLUMNS.index("mrlratio")]) if row[COLUMNS.index("mrlratio")] else None
        row[COLUMNS.index("callday")] = int(row[COLUMNS.index("callday")]) if row[COLUMNS.index("callday")] else None
        row[COLUMNS.index("limitdaily")] = float(row[COLUMNS.index("limitdaily")]) if row[COLUMNS.index("limitdaily")] else None
        row[COLUMNS.index("mrcrate")] = float(row[COLUMNS.index("mrcrate")]) if row[COLUMNS.index("mrcrate")] else None
        row[COLUMNS.index("mrwrate")] = float(row[COLUMNS.index("mrwrate")]) if row[COLUMNS.index("mrwrate")] else None
        row[COLUMNS.index("k1days")] = int(row[COLUMNS.index("k1days")]) if row[COLUMNS.index("k1days")] else None
        row[COLUMNS.index("k2days")] = int(row[COLUMNS.index("k2days")]) if row[COLUMNS.index("k2days")] else None
        row[COLUMNS.index("mrexrate")] = float(row[COLUMNS.index("mrexrate")]) if row[COLUMNS.index("mrexrate")] else None
        row[COLUMNS.index("tradeline")] = float(row[COLUMNS.index("tradeline")]) if row[COLUMNS.index("tradeline")] else None
        row[COLUMNS.index("clamtlimit")] = float(row[COLUMNS.index("clamtlimit")]) if row[COLUMNS.index("clamtlimit")] else None
    return rows
def write_error_log(row, index, error_msg):
    with open("insert_AFMAST_error.log", "a", encoding="utf-8") as f:
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
    INSERT INTO afmast (
        {', '.join(COLUMNS)}
    ) VALUES ({', '.join(['?' for _ in range(NUM_COLUMNS)])})
    """
    prepared = session.prepare(insert_stmt)
    for i in range(0, len(rows), batch_size):
        batch_rows = rows[i:i + batch_size]
        indices = list(range(i + 1, i + 1 + len(batch_rows)))  # Dòng bắt đầu từ 1
        insert_batch_recursive(prepared, batch_rows, indices)
    print("⚡ Bulk insert finished. Check insert_AFMAST_error.log for any failed rows.")
if __name__ == "__main__":
    if os.path.exists("insert_AFMAST_error.log"):
        os.remove("insert_AFMAST_error.log")
    rows = parse_sql_file("afm.sql")
    bulk_insert(rows)