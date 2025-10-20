

-- 1) Kết nối tới nhiều loại DB
CREATE TABLE connection_ctrl
(
    id             BIGSERIAL PRIMARY KEY,
    database_name  VARCHAR(50)  NOT NULL,           -- 'postgresql','mysql','sqlserver','oracle','sqlite','mongodb','cassandra','redis','clickhouse','snowflake','bigquery','redshift','dynamodb','neo4j', ...
    name           VARCHAR(100) NOT NULL UNIQUE,    -- Tên thân thiện: "prod-pg", "dev-mongo"...
    host           VARCHAR(255),
    port           INT,
    username       VARCHAR(128),
    password       TEXT,                            -- hoặc để trống, hoặc lưu ref tới secret manager
    auth_method    VARCHAR(30) DEFAULT 'basic',     -- 'basic','oauth','kerberos','iam','none'
    connect_string TEXT,                            -- DSN/URI đầy đủ nếu dùng
    ssl_enabled    BOOLEAN     DEFAULT FALSE,
    options_params JSONB       DEFAULT '{}'::jsonb, -- tuỳ chọn khác (timeout, options…)
    description    TEXT,
    is_active      BOOLEAN     DEFAULT TRUE,
    created_at     TIMESTAMPTZ DEFAULT NOW(),
    updated_at     TIMESTAMPTZ DEFAULT NOW(),
    CONSTRAINT connection_database_name_chk CHECK (database_name <> '')
);


-- 2) Datastore: database / keyspace / catalog / bucket / project / dataset...
CREATE TABLE datastore_ctrl
(
    id            BIGSERIAL PRIMARY KEY,
    connection_id BIGINT       NOT NULL REFERENCES connection_ctrl (id) ON DELETE CASCADE,
    name          VARCHAR(128) NOT NULL,
    store_type    VARCHAR(30)  NOT NULL, -- 'database','keyspace','catalog','bucket','project','dataset','index'
    description   TEXT,
    attributes    JSONB DEFAULT '{}'::jsonb,
    created_at     TIMESTAMPTZ DEFAULT NOW(),
    updated_at     TIMESTAMPTZ DEFAULT NOW(),
    UNIQUE (connection_id, name, store_type)
);
CREATE INDEX ON datastore_ctrl(connection_id);


-- 3) Schema / Namespace (có thể không dùng với NoSQL)
CREATE TABLE schema_ctrl
(
    id           BIGSERIAL PRIMARY KEY,
    datastore_id BIGINT       NOT NULL /*REFERENCES datastore_ctrl (id) ON DELETE CASCADE*/,
    name         VARCHAR(128) NOT NULL,
    description  TEXT,
    attributes   JSONB DEFAULT '{}'::jsonb,
    created_at     TIMESTAMPTZ DEFAULT NOW(),
    updated_at     TIMESTAMPTZ DEFAULT NOW(),
    UNIQUE (datastore_id, name)
);
CREATE INDEX ON schema_ctrl(datastore_id);


-- 4) Entity: bảng/collection/view/materialized_view/column_family/topic/file...
CREATE TABLE entity_ctrl
(
    id           BIGSERIAL PRIMARY KEY,
    datastore_id BIGINT       NOT NULL /*REFERENCES datastore_ctrl (id) ON DELETE CASCADE*/,
    schema_id    BIGINT       NOT NULL /*REFERENCES schema_ctrl (id) ON DELETE SET NULL*/,   -- NULL nếu hệ không có schema
    name         VARCHAR(256) NOT NULL,
    entity_type  VARCHAR(30)  NOT NULL,                                             -- 'table','view','materialized_view','collection','column_family','graph','index','stream','topic','file','other'
    description  TEXT,
    attributes   JSONB DEFAULT '{}'::jsonb,
    created_at     TIMESTAMPTZ DEFAULT NOW(),
    updated_at     TIMESTAMPTZ DEFAULT NOW(),
    -- tuỳ hệ, cặp (datastore_id, schema_id, name) nên duy nhất; với schema_id NULL thì dùng unique partial:
    UNIQUE (datastore_id, schema_id, name)
);
CREATE INDEX ON entity_ctrl(datastore_id);
CREATE INDEX ON entity_ctrl(schema_id);


-- 5) Field / Column metadata
CREATE TABLE field_ctrl
(
    id               BIGSERIAL PRIMARY KEY,
    entity_id        BIGINT       NOT NULL /*REFERENCES entity_ctrl (id) ON DELETE CASCADE*/,
    name             VARCHAR(256) NOT NULL,
    data_type        VARCHAR(64)  NOT NULL,       -- logical type: VARCHAR, INT, TIMESTAMP, JSON, OBJECT, ARRAY...
    length           INT,
    precision        INT,
    scale            INT,
    is_nullable      BOOLEAN DEFAULT TRUE,
    is_primary_key   BOOLEAN DEFAULT FALSE,
    is_unique        BOOLEAN DEFAULT FALSE,
    default_expr     TEXT,                        -- biểu thức hoặc literal mặc định
    description      TEXT,
    attributes       JSONB   DEFAULT '{}'::jsonb, -- đặc thù hệ: charset, collation, encoding, shard key part...
    ordinal_position INT,
    created_at     TIMESTAMPTZ DEFAULT NOW(),
    updated_at     TIMESTAMPTZ DEFAULT NOW(),
    UNIQUE (entity_id, name)
);
CREATE INDEX ON field_ctrl(entity_id);


-- 6) Khai báo quan hệ ĐẦU VÀO/ĐẦU RA và PHÉP GỘP (join/union/lookup...) giữa entities
CREATE TABLE entity_relation_ctrl
(
    id               BIGSERIAL PRIMARY KEY,
    target_entity_id BIGINT      NOT NULL /*REFERENCES entity_ctrl (id) ON DELETE CASCADE*/,    -- entity đích (kết quả)
    source_entity_id BIGINT      NOT NULL /*REFERENCES entity_ctrl (id) ON DELETE CASCADE*/,    -- entity nguồn
    relation_kind    VARCHAR(20) NOT NULL,                                                      -- 'join','union','lookup','source','sink','dependency'
    join_type        VARCHAR(10),                                                               -- 'inner','left','right','full','cross' (nếu relation_kind='join')
    join_condition   TEXT,                                                                      -- điều kiện join
    filter_condition TEXT,                                                                      -- where/filters áp dụng cho source khi build target
    projection       JSONB,                                                                     -- danh sách cột chọn/mapping (vd: [{"as":"u_id","expr":"u.id"}...])
    order_by         TEXT,
    attributes       JSONB DEFAULT '{}'::jsonb,
    notes            TEXT,
    created_at     TIMESTAMPTZ DEFAULT NOW(),
    updated_at     TIMESTAMPTZ DEFAULT NOW()
);
CREATE INDEX ON entity_relation_ctrl(target_entity_id);
CREATE INDEX ON entity_relation_ctrl(source_entity_id);


-- 7) Khai báo ĐẦU RA (sink): API/Table/File/Topic/Queue...
CREATE TABLE sink_ctrl
(
    id          BIGSERIAL PRIMARY KEY,
    name        VARCHAR(128) NOT NULL,                                                      -- tên cấu hình output
    sink_type   VARCHAR(20)  NOT NULL,                                                      -- 'api','table','file','topic','queue','view'
    entity_id   BIGINT       NOT NULL /*REFERENCES entity_ctrl (id) ON DELETE CASCADE*/,    -- entity nguồn để xuất ra
    target_path TEXT,                                                                       -- ví dụ: '/api/v1/users', 's3://bucket/path', 'kafka:topic', 'db.schema.table'
    http_method VARCHAR(10),                                                                -- nếu sink_type='api': GET/POST/PUT/DELETE
    mapping     JSONB,                                                                      -- field mapping cho output
    attributes  JSONB DEFAULT '{}'::jsonb,                                                  -- headers, auth, format (csv/json/avro/parquet)...
    description TEXT,
    created_at     TIMESTAMPTZ DEFAULT NOW(),
    updated_at     TIMESTAMPTZ DEFAULT NOW(),
);
CREATE INDEX ON sink_ctrl(entity_id);




