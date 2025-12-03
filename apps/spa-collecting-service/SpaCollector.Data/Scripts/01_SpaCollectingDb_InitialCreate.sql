
-- Sitemap : Defines a url & regex filter for a source websites

create table if not exists sitemaps (
    id uuid primary key,
    sku_id uuid not null,
    url varchar(500) not null,
    regex_filter varchar(500) not null,
    is_active bool not null default(true),
    created_at timestamptz not null default now(),
    created_by varchar(255) not null,
    updated_at timestamptz not null default now(),
    updated_by varchar(255) not null
);

-- SpaConfig : Defines how collection for a sku should be performed

create table if not exists spa_configs (
    id uuid primary key,
    sku_id uuid not null,
    user_agent varchar(255),
    max_concurrency int not null,
    created_at timestamptz not null default now(),
    created_by varchar(255) not null,
    updated_at timestamptz not null default now(),
    updated_by varchar(255) not null
);

create unique index if not exists ix_spa_configs_sku_id
    on spa_configs(sku_id);
