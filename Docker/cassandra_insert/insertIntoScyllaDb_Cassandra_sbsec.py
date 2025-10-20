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
    "codeid", "issuerid", "symbol", "sectype", "investmenttype", "risktype", "parvalue", "foreignrate", "status",
    "tradeplace", "depository", "securedratio", "mortageratio", "reporatio", "issuedate", "expdate", "intperiod",
    "intrate", "halt", "sbtype", "careby", "chkrate", "refcodeid", "issqtty", "bondtype", "markettype", "allowsession",
    "issedepofee", "intcoupon", "typeterm", "term", "intperiodcd", "tradeqttylot", "tradeqttymin", "tradeamtmin",
    "ccycd", "expdatetmp", "sbtotalamt", "sbcirculatetodate", "sbtotalcirculate", "sbpayintdate", "sbperiodpayint",
    "sbfromdate", "sbtodate", "sbnotes", "sbreportdate", "sbintratedate", "sbduedate", "sbothdate", "bankacc",
    "bankname", "citybank", "mratio", "bratio", "chstatus", "pstatus", "underlyingtype", "underlyingsymbol",
    "issuername", "coveredwarranttype", "settlementtype", "settlementprice", "cwterm", "maturitydate", "lasttradingdate",
    "nvalue", "exerciseprice", "exerciseratio", "isincode", "odd_lot_halt", "trscope", "domain"
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
        row[COLUMNS.index("parvalue")] = float(row[COLUMNS.index("parvalue")]) if row[COLUMNS.index("parvalue")] else None
        row[COLUMNS.index("foreignrate")] = float(row[COLUMNS.index("foreignrate")]) if row[COLUMNS.index("foreignrate")] else None
        row[COLUMNS.index("securedratio")] = float(row[COLUMNS.index("securedratio")]) if row[COLUMNS.index("securedratio")] else None
        row[COLUMNS.index("mortageratio")] = float(row[COLUMNS.index("mortageratio")]) if row[COLUMNS.index("mortageratio")] else None
        row[COLUMNS.index("reporatio")] = float(row[COLUMNS.index("reporatio")]) if row[COLUMNS.index("reporatio")] else None
        row[COLUMNS.index("intrate")] = float(row[COLUMNS.index("intrate")]) if row[COLUMNS.index("intrate")] else None
        row[COLUMNS.index("intperiod")] = int(row[COLUMNS.index("intperiod")]) if row[COLUMNS.index("intperiod")] else None
        row[COLUMNS.index("chkrate")] = float(row[COLUMNS.index("chkrate")]) if row[COLUMNS.index("chkrate")] else None
        row[COLUMNS.index("issqtty")] = float(row[COLUMNS.index("issqtty")]) if row[COLUMNS.index("issqtty")] else None
        row[COLUMNS.index("intcoupon")] = float(row[COLUMNS.index("intcoupon")]) if row[COLUMNS.index("intcoupon")] else None
        row[COLUMNS.index("term")] = int(row[COLUMNS.index("term")]) if row[COLUMNS.index("term")] else None
        row[COLUMNS.index("tradeqttylot")] = float(row[COLUMNS.index("tradeqttylot")]) if row[COLUMNS.index("tradeqttylot")] else None
        row[COLUMNS.index("tradeqttymin")] = float(row[COLUMNS.index("tradeqttymin")]) if row[COLUMNS.index("tradeqttymin")] else None
        row[COLUMNS.index("tradeamtmin")] = float(row[COLUMNS.index("tradeamtmin")]) if row[COLUMNS.index("tradeamtmin")] else None
        row[COLUMNS.index("sbtotalamt")] = float(row[COLUMNS.index("sbtotalamt")]) if row[COLUMNS.index("sbtotalamt")] else None
        row[COLUMNS.index("sbtotalcirculate")] = float(row[COLUMNS.index("sbtotalcirculate")]) if row[COLUMNS.index("sbtotalcirculate")] else None
        row[COLUMNS.index("sbperiodpayint")] = int(row[COLUMNS.index("sbperiodpayint")]) if row[COLUMNS.index("sbperiodpayint")] else None
        row[COLUMNS.index("mratio")] = float(row[COLUMNS.index("mratio")]) if row[COLUMNS.index("mratio")] else None
        row[COLUMNS.index("bratio")] = float(row[COLUMNS.index("bratio")]) if row[COLUMNS.index("bratio")] else None
        row[COLUMNS.index("settlementprice")] = float(row[COLUMNS.index("settlementprice")]) if row[COLUMNS.index("settlementprice")] else None
        row[COLUMNS.index("cwterm")] = int(row[COLUMNS.index("cwterm")]) if row[COLUMNS.index("cwterm")] else None
        row[COLUMNS.index("nvalue")] = float(row[COLUMNS.index("nvalue")]) if row[COLUMNS.index("nvalue")] else None
        row[COLUMNS.index("exerciseprice")] = float(row[COLUMNS.index("exerciseprice")]) if row[COLUMNS.index("exerciseprice")] else None
        row[COLUMNS.index("exerciseratio")] = float(row[COLUMNS.index("exerciseratio")]) if row[COLUMNS.index("exerciseratio")] else None
        try:
            row[COLUMNS.index("trscope")] = int(row[COLUMNS.index("trscope")]) if row[COLUMNS.index("trscope")] else None
        except ValueError:
            row[COLUMNS.index("trscope")] = None
    return rows
def write_error_log(row, index, error_msg):
    with open("insert_SBSECURITIES_error.log", "a", encoding="utf-8") as f:
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
    INSERT INTO sbsecurities (
        {', '.join(COLUMNS)}
    ) VALUES ({', '.join(['?' for _ in range(NUM_COLUMNS)])})
    """
    prepared = session.prepare(insert_stmt)
    for i in range(0, len(rows), batch_size):
        batch_rows = rows[i:i + batch_size]
        indices = list(range(i + 1, i + 1 + len(batch_rows)))  # Dòng bắt đầu từ 1
        insert_batch_recursive(prepared, batch_rows, indices)
    print("⚡ Bulk insert finished. Check insert_SBSECURITIES_error.log for any failed rows.")
if __name__ == "__main__":
    if os.path.exists("insert_SBSECURITIES_error.log"):
        os.remove("insert_SBSECURITIES_error.log")
    rows = parse_sql_file("sbsec.sql")
    bulk_insert(rows)