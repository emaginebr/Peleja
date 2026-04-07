-- =============================================
-- Peleja Database Schema
-- PostgreSQL (single database)
-- =============================================

-- Table: peleja_sites
CREATE TABLE peleja_sites (
    site_id    BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    client_id  VARCHAR(32)    NOT NULL,
    site_url   VARCHAR(2000)  NOT NULL,
    tenant     VARCHAR(100)   NOT NULL,
    user_id    BIGINT         NOT NULL,
    status     INTEGER        NOT NULL DEFAULT 1,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    updated_at TIMESTAMP WITHOUT TIME ZONE
);

CREATE UNIQUE INDEX ix_peleja_sites_client_id ON peleja_sites (client_id);
CREATE UNIQUE INDEX ix_peleja_sites_site_url ON peleja_sites (site_url);
CREATE INDEX ix_peleja_sites_user_id ON peleja_sites (user_id);

-- Table: peleja_pages
CREATE TABLE peleja_pages (
    page_id    BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    site_id    BIGINT         NOT NULL,
    page_url   VARCHAR(2000)  NOT NULL,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    updated_at TIMESTAMP WITHOUT TIME ZONE
);

CREATE UNIQUE INDEX ix_peleja_pages_site_page_url ON peleja_pages (site_id, page_url);

-- Table: peleja_comments
CREATE TABLE peleja_comments (
    comment_id        BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    page_id           BIGINT         NOT NULL,
    user_id           BIGINT         NOT NULL,
    parent_comment_id BIGINT,
    content           VARCHAR(5000)  NOT NULL,
    gif_url           VARCHAR(2000),
    like_count        INTEGER        NOT NULL DEFAULT 0,
    is_edited         BOOLEAN        NOT NULL DEFAULT FALSE,
    is_deleted        BOOLEAN        NOT NULL DEFAULT FALSE,
    created_at        TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    updated_at        TIMESTAMP WITHOUT TIME ZONE,
    deleted_at        TIMESTAMP WITHOUT TIME ZONE,

    CONSTRAINT fk_peleja_pages_comments
        FOREIGN KEY (page_id) REFERENCES peleja_pages (page_id)
        ON DELETE SET NULL,

    CONSTRAINT fk_peleja_comments_comments
        FOREIGN KEY (parent_comment_id) REFERENCES peleja_comments (comment_id)
        ON DELETE SET NULL
);

CREATE INDEX ix_peleja_comments_page
    ON peleja_comments (page_id, is_deleted);

CREATE INDEX ix_peleja_comments_popular
    ON peleja_comments (page_id, like_count DESC, comment_id DESC)
    WHERE is_deleted = FALSE AND parent_comment_id IS NULL;

CREATE INDEX ix_peleja_comments_recent
    ON peleja_comments (page_id, comment_id DESC)
    WHERE is_deleted = FALSE AND parent_comment_id IS NULL;

CREATE INDEX ix_peleja_comments_parent
    ON peleja_comments (parent_comment_id)
    WHERE parent_comment_id IS NOT NULL;

-- Table: peleja_comment_likes
CREATE TABLE peleja_comment_likes (
    comment_like_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    comment_id      BIGINT NOT NULL,
    user_id         BIGINT NOT NULL,
    created_at      TIMESTAMP WITHOUT TIME ZONE NOT NULL,

    CONSTRAINT fk_peleja_comments_comment_likes
        FOREIGN KEY (comment_id) REFERENCES peleja_comments (comment_id)
        ON DELETE SET NULL
);

CREATE UNIQUE INDEX ix_peleja_comment_likes_unique ON peleja_comment_likes (comment_id, user_id);
