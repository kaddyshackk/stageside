-- Create Quartz.NET PostgreSQL Schema
-- Modified to use dedicated 'quartz' schema

-- Create the quartz schema
CREATE SCHEMA IF NOT EXISTS quartz;

-- Set search_path to use the quartz schema
SET search_path TO quartz, public;

-- Drop existing tables if they exist
DROP TABLE IF EXISTS quartz.qrtz_fired_triggers;
DROP TABLE IF EXISTS quartz.qrtz_paused_trigger_grps;
DROP TABLE IF EXISTS quartz.qrtz_scheduler_state;
DROP TABLE IF EXISTS quartz.qrtz_locks;
DROP TABLE IF EXISTS quartz.qrtz_simple_triggers;
DROP TABLE IF EXISTS quartz.qrtz_simprop_triggers;
DROP TABLE IF EXISTS quartz.qrtz_cron_triggers;
DROP TABLE IF EXISTS quartz.qrtz_blob_triggers;
DROP TABLE IF EXISTS quartz.qrtz_triggers;
DROP TABLE IF EXISTS quartz.qrtz_job_details;
DROP TABLE IF EXISTS quartz.qrtz_calendars;

-- Create Quartz tables in the quartz schema
CREATE TABLE quartz.qrtz_job_details (
    sched_name varchar(120) not null,
    job_name varchar(200) not null,
    job_group varchar(200) not null,
    description varchar(250) null,
    job_class_name varchar(250) not null,
    is_durable bool not null,
    is_nonconcurrent bool not null,
    is_update_data bool not null,
    requests_recovery bool not null,
    job_data bytea null,
    primary key (sched_name, job_name, job_group)
);

CREATE TABLE quartz.qrtz_triggers (
    sched_name varchar(120) not null,
    trigger_name varchar(200) not null,
    trigger_group varchar(200) not null,
    job_name varchar(200) not null,
    job_group varchar(200) not null,
    description varchar(250) null,
    next_fire_time bigint null,
    prev_fire_time bigint null,
    priority integer null,
    trigger_state varchar(16) not null,
    trigger_type varchar(8) not null,
    start_time bigint not null,
    end_time bigint null,
    calendar_name varchar(200) null,
    misfire_instr smallint null,
    job_data bytea null,
    primary key (sched_name, trigger_name, trigger_group),
    foreign key (sched_name, job_name, job_group)
        references quartz.qrtz_job_details(sched_name, job_name, job_group)
);

CREATE TABLE quartz.qrtz_simple_triggers (
    sched_name varchar(120) not null,
    trigger_name varchar(200) not null,
    trigger_group varchar(200) not null,
    repeat_count bigint not null,
    repeat_interval bigint not null,
    times_triggered bigint not null,
    primary key (sched_name, trigger_name, trigger_group),
    foreign key (sched_name, trigger_name, trigger_group)
        references quartz.qrtz_triggers(sched_name, trigger_name, trigger_group)
);

CREATE TABLE quartz.qrtz_cron_triggers (
    sched_name varchar(120) not null,
    trigger_name varchar(200) not null,
    trigger_group varchar(200) not null,
    cron_expression varchar(120) not null,
    time_zone_id varchar(80),
    primary key (sched_name, trigger_name, trigger_group),
    foreign key (sched_name, trigger_name, trigger_group)
        references quartz.qrtz_triggers(sched_name, trigger_name, trigger_group)
);

CREATE TABLE quartz.qrtz_simprop_triggers (
    sched_name varchar(120) not null,
    trigger_name varchar(200) not null,
    trigger_group varchar(200) not null,
    str_prop_1 varchar(512) null,
    str_prop_2 varchar(512) null,
    str_prop_3 varchar(512) null,
    int_prop_1 int null,
    int_prop_2 int null,
    long_prop_1 bigint null,
    long_prop_2 bigint null,
    dec_prop_1 numeric(13,4) null,
    dec_prop_2 numeric(13,4) null,
    bool_prop_1 bool null,
    bool_prop_2 bool null,
    primary key (sched_name, trigger_name, trigger_group),
    foreign key (sched_name, trigger_name, trigger_group)
        references quartz.qrtz_triggers(sched_name, trigger_name, trigger_group)
);

CREATE TABLE quartz.qrtz_blob_triggers (
    sched_name varchar(120) not null,
    trigger_name varchar(200) not null,
    trigger_group varchar(200) not null,
    blob_data bytea null,
    primary key (sched_name, trigger_name, trigger_group),
    foreign key (sched_name, trigger_name, trigger_group)
        references quartz.qrtz_triggers(sched_name, trigger_name, trigger_group)
);

CREATE TABLE quartz.qrtz_calendars (
    sched_name varchar(120) not null,
    calendar_name varchar(200) not null,
    calendar bytea not null,
    primary key (sched_name, calendar_name)
);

CREATE TABLE quartz.qrtz_paused_trigger_grps (
    sched_name varchar(120) not null,
    trigger_group varchar(200) not null,
    primary key (sched_name, trigger_group)
);

CREATE TABLE quartz.qrtz_fired_triggers (
    sched_name varchar(120) not null,
    entry_id varchar(95) not null,
    trigger_name varchar(200) not null,
    trigger_group varchar(200) not null,
    instance_name varchar(200) not null,
    fired_time bigint not null,
    sched_time bigint not null,
    priority integer not null,
    state varchar(16) not null,
    job_name varchar(200) null,
    job_group varchar(200) null,
    is_nonconcurrent bool null,
    requests_recovery bool null,
    primary key (sched_name, entry_id)
);

CREATE TABLE quartz.qrtz_scheduler_state (
    sched_name varchar(120) not null,
    instance_name varchar(200) not null,
    last_checkin_time bigint not null,
    checkin_interval bigint not null,
    primary key (sched_name, instance_name)
);

CREATE TABLE quartz.qrtz_locks (
    sched_name varchar(120) not null,
    lock_name varchar(40) not null,
    primary key (sched_name, lock_name)
);

-- Create indexes for better performance
CREATE INDEX idx_qrtz_j_req_recovery ON quartz.qrtz_job_details(sched_name, requests_recovery);
CREATE INDEX idx_qrtz_j_grp ON quartz.qrtz_job_details(sched_name, job_group);

CREATE INDEX idx_qrtz_t_j ON quartz.qrtz_triggers(sched_name, job_name, job_group);
CREATE INDEX idx_qrtz_t_jg ON quartz.qrtz_triggers(sched_name, job_group);
CREATE INDEX idx_qrtz_t_c ON quartz.qrtz_triggers(sched_name, calendar_name);
CREATE INDEX idx_qrtz_t_g ON quartz.qrtz_triggers(sched_name, trigger_group);
CREATE INDEX idx_qrtz_t_state ON quartz.qrtz_triggers(sched_name, trigger_state);
CREATE INDEX idx_qrtz_t_n_state ON quartz.qrtz_triggers(sched_name, trigger_name, trigger_group, trigger_state);
CREATE INDEX idx_qrtz_t_n_g_state ON quartz.qrtz_triggers(sched_name, trigger_group, trigger_state);
CREATE INDEX idx_qrtz_t_next_fire_time ON quartz.qrtz_triggers(sched_name, next_fire_time);
CREATE INDEX idx_qrtz_t_nft_st ON quartz.qrtz_triggers(sched_name, trigger_state, next_fire_time);
CREATE INDEX idx_qrtz_t_nft_misfire ON quartz.qrtz_triggers(sched_name, misfire_instr, next_fire_time);
CREATE INDEX idx_qrtz_t_nft_st_misfire ON quartz.qrtz_triggers(sched_name, misfire_instr, next_fire_time, trigger_state);
CREATE INDEX idx_qrtz_t_nft_st_misfire_grp ON quartz.qrtz_triggers(sched_name, misfire_instr, next_fire_time, trigger_group, trigger_state);

CREATE INDEX idx_qrtz_ft_trig_inst_name ON quartz.qrtz_fired_triggers(sched_name, instance_name);
CREATE INDEX idx_qrtz_ft_inst_job_req_rcvry ON quartz.qrtz_fired_triggers(sched_name, instance_name, requests_recovery);
CREATE INDEX idx_qrtz_ft_j_g ON quartz.qrtz_fired_triggers(sched_name, job_name, job_group);
CREATE INDEX idx_qrtz_ft_jg ON quartz.qrtz_fired_triggers(sched_name, job_group);
CREATE INDEX idx_qrtz_ft_t_g ON quartz.qrtz_fired_triggers(sched_name, trigger_name, trigger_group);
CREATE INDEX idx_qrtz_ft_tg ON quartz.qrtz_fired_triggers(sched_name, trigger_group);

-- Reset search_path to default
SET search_path TO public;