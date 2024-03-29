
-- Create tables
CREATE TABLE school_user 
( 
    "id" INTEGER GENERATED BY DEFAULT AS IDENTITY UNIQUE PRIMARY KEY,
    "name" VARCHAR
); 

CREATE TABLE study_group 
( 
    "id" INTEGER GENERATED BY DEFAULT AS IDENTITY UNIQUE PRIMARY KEY,
    "subject" VARCHAR,
    "created_at" TIMESTAMP DEFAULT (NOW())
    CONSTRAINT allowed_subjects CHECK ("subject" IN ('Math', 'Chemistry', 'Physics'))
);

CREATE TABLE group_assignment
(
    "user_id" INTEGER,
    "study_group_id" INTEGER
);

-- Configure FKs
ALTER TABLE "group_assignment" ADD FOREIGN KEY ("user_id") REFERENCES "school_user" ("id");
ALTER TABLE "group_assignment" ADD FOREIGN KEY ("study_group_id") REFERENCES "study_group" ("id");

-- Add entities
INSERT INTO school_user ("name") VALUES ('John Snow'), ('John Snorkel'), ('Mathias'), ('Miguel'), ('Manuel'), ('Pedro');

INSERT INTO study_group ("subject") VALUES ('Math'), ('Chemistry'), ('Physics'), ('Math'), ('Chemistry'), ('Physics');

INSERT INTO group_assignment ("user_id", "study_group_id") VALUES
(1, 1), -- John Snow - Math 1
(2, 1), -- John Snorkel - Math 1
(3, 1), -- Mathias - Math 1
(4, 1), -- Miguel - Math 1
(5, 1), -- Manuel - Math 1
(6, 1), -- Pedro - Math 1
(2, 2), -- John Snorkel - Chemistry 1
(3, 2), -- Mathias - Chemistry 1
(1, 3), -- John Snow - Physics 1
(2, 3), -- John Snorkel - Physics 1
(6, 4), -- Pedro - Math 2
(5, 4)  -- Manuel - Math 2
;

-- Groups which have a user with a naming started by M: Math 1 (id: 1), Chemistry 1 (id: 2), Math 2 (id: 4)

-- Select study group that contains at least one user with a name starting with 'M', sorted by newest first

-- Option 1: Slower, easier to maintain
SELECT
	study_group.id,
    study_group.subject,
    study_group.created_at
FROM study_group
INNER JOIN group_assignment ON group_assignment.study_group_id = study_group.id
INNER JOIN school_user ON group_assignment.user_id = school_user.id
WHERE LOWER(school_user.name) LIKE 'm%'
GROUP BY (study_group.id)
ORDER BY (study_group.created_at) ASC;

-- Option 2: Faster, harder to maintain
SELECT
    *
FROM study_group as sg
WHERE sg.id IN (
    SELECT DISTINCT
        ga.study_group_id
    FROM group_assignment AS ga
    WHERE ga.user_id IN (
        SELECT
            usr.id
        FROM school_user as usr
        WHERE LOWER(usr.name) LIKE 'm%'
    )
)
ORDER BY (sg.created_at) DESC;

-- For much faster query times we can use a B-Tree indexing strategy for school_user.name
