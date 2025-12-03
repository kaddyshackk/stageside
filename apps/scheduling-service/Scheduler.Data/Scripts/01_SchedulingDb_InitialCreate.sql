
-- Source : Defines a website or api from which we are collecting data

create table if not exists sources (
    id uuid primary key,
    name varchar(255) not null,
    website varchar(255) not null,
    is_active bool default(true),
    created_at timestamptz not null default now(),
    created_by varchar(255) not null,
    updated_at timestamptz not null default now(),
    updated_by varchar(255) not null
);

-- Sku : Defines a type of data or resource that can be collected from a source

create table if not exists skus (
    id uuid primary key,
    source_id uuid not null,
    name varchar(255) not null,
    type varchar(100) not null,
    is_active bool default(true),
    created_at timestamptz not null default now(),
    created_by varchar(255) not null,
    updated_at timestamptz not null default now(),
    updated_by varchar(255) not null,
    constraint fk_skus_sources
        foreign key (source_id)
        references sources(id)
        on delete cascade
);

-- Schedule : Defines a one-time or repeated collection of a specific sku

create table if not exists schedules (
    id uuid primary key,
    sku_id uuid not null,
    source_id uuid not null,
    name varchar(255) not null,
    cron_expression varchar(50),
    next_execution timestamptz not null,
    last_execution timestamptz,
    is_active bool default(true),
    created_at timestamptz not null default now(),
    created_by varchar(255) not null,
    updated_at timestamptz not null default now(),
    updated_by varchar(255) not null,
    constraint fk_schedules_skus
        foreign key (sku_id)
        references skus(id)
        on delete cascade,
    constraint fk_schedules_sources
        foreign key (source_id)
        references sources(id)
        on delete cascade
);

create index if not exists ix_schedules_nextexecution
    on schedules(next_execution);

create index if not exists ix_schedules_isactive_nextexecution
    on schedules(is_active, next_execution);

-- Job : Defines an execution of a specific schedule

create table if not exists jobs (
    id uuid primary key,
    schedule_id uuid not null,
    status varchar(50) not null,
    started_at timestamptz,
    completed_at timestamptz,
    error_message varchar(255),
    created_at timestamptz not null default now(),
    created_by varchar(255) not null,
    updated_at timestamptz not null default now(),
    updated_by varchar(255) not null,
    constraint fk_jobs_schedules
        foreign key (schedule_id)
        references schedules(id)
        on delete cascade
);
