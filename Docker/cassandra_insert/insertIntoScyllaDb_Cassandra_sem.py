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
    "actype", "acctno", "codeid", "afacctno", "opndate", "clsdate", "lastdate", "status", "pstatus", "irtied", "ircd",
    "costprice", "trade", "mortage", "margin", "netting", "standing", "withdraw", "deposit", "loan", "blocked",
    "receiving", "transfer", "prevqtty", "dcrqtty", "dcramt", "depofeeacr", "repo", "pending", "tbaldepo", "custid",
    "costdt", "secured", "iccfcd", "iccftied", "tbaldt", "senddeposit", "sendpending", "ddroutqtty", "ddroutamt",
    "dtoclose", "sdtoclose", "qtty_transfer", "last_change", "dealintpaid", "wtrade", "grpordamt", "emkqtty",
    "blockwithdraw", "blockdtoclose", "roomchk", "roomlimit", "costprice_adj_date", "shareholdersid", "oldshareholdersid"
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
    return rows

def write_error_log(row, index, error_msg):
    with open("insert_SEMAST_error.log", "a", encoding="utf-8") as f:
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
    INSERT INTO semast (
        {', '.join(COLUMNS)}
    ) VALUES ({', '.join(['?' for _ in range(NUM_COLUMNS)])})
    """
    prepared = session.prepare(insert_stmt)

    for i in range(0, len(rows), batch_size):
        batch_rows = rows[i:i + batch_size]
        indices = list(range(i + 1, i + 1 + len(batch_rows)))  # Dòng bắt đầu từ 1
        insert_batch_recursive(prepared, batch_rows, indices)

    print("⚡ Bulk insert finished. Check insert_error.log for any failed rows.")

if __name__ == "__main__":
    if os.path.exists("insert_SEMAST_error.log"):
        os.remove("insert_SEMAST_error.log")

    rows = parse_sql_file("sem.sql")
    bulk_insert(rows)
