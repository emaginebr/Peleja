-- =============================================
-- Peleja Database Schema
-- PostgreSQL (per-tenant database)
-- =============================================

-- =============================================
-- Table: pages
-- =============================================
CREATE TABLE pages (
    page_id    BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id    BIGINT         NOT NULL,
    page_url   VARCHAR(2000)  NOT NULL,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    updated_at TIMESTAMP WITHOUT TIME ZONE
);

CREATE UNIQUE INDEX ix_pages_page_url ON pages (page_url);

-- =============================================
-- Table: comments
-- =============================================
CREATE TABLE comments (
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

    CONSTRAINT fk_pages_comments
        FOREIGN KEY (page_id) REFERENCES pages (page_id)
        ON DELETE SET NULL,

    CONSTRAINT fk_comments_comments
        FOREIGN KEY (parent_comment_id) REFERENCES comments (comment_id)
        ON DELETE SET NULL
);

CREATE INDEX ix_comments_page
    ON comments (page_id, is_deleted);

CREATE INDEX ix_comments_popular
    ON comments (page_id, like_count DESC, comment_id DESC)
    WHERE is_deleted = FALSE AND parent_comment_id IS NULL;

CREATE INDEX ix_comments_recent
    ON comments (page_id, comment_id DESC)
    WHERE is_deleted = FALSE AND parent_comment_id IS NULL;

CREATE INDEX ix_comments_parent
    ON comments (parent_comment_id)
    WHERE parent_comment_id IS NOT NULL;

-- =============================================
-- Table: comment_likes
-- =============================================
CREATE TABLE comment_likes (
    comment_like_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    comment_id      BIGINT NOT NULL,
    user_id         BIGINT NOT NULL,
    created_at      TIMESTAMP WITHOUT TIME ZONE NOT NULL,

    CONSTRAINT fk_comments_comment_likes
        FOREIGN KEY (comment_id) REFERENCES comments (comment_id)
        ON DELETE SET NULL
);

CREATE UNIQUE INDEX ix_comment_likes_unique ON comment_likes (comment_id, user_id);
