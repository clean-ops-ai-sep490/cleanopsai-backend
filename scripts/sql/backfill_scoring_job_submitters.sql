-- Manual backfill for legacy scoring jobs whose submitted_by_user_id is missing or wrong.
-- Update the VALUES list with explicit (job_id, worker_user_id) pairs before running.
-- This script validates that:
-- 1. the scoring job exists and is not deleted
-- 2. the worker exists and is not deleted
-- 3. the worker has at least one active supervisor assignment

BEGIN;

DO $$
DECLARE
	invalid_count integer;
BEGIN
	WITH input(job_id, worker_user_id) AS (
		VALUES
			('00000000-0000-0000-0000-000000000000'::uuid, '00000000-0000-0000-0000-000000000000'::uuid)
	),
	validation AS (
		SELECT
			i.job_id,
			i.worker_user_id,
			sj.id AS existing_job_id,
			w.id AS existing_worker_id,
			EXISTS (
				SELECT 1
				FROM workforce.workarea_supervisors was
				WHERE was.worker_id = w.id
					AND was.is_deleted = false
			) AS has_supervisor_assignment
		FROM input i
		LEFT JOIN scoring.scoring_jobs sj
			ON sj.id = i.job_id
			AND sj.is_deleted = false
		LEFT JOIN workforce.workers w
			ON w.user_id = i.worker_user_id
			AND w.is_deleted = false
	)
	SELECT COUNT(*)
	INTO invalid_count
	FROM validation
	WHERE existing_job_id IS NULL
		OR existing_worker_id IS NULL
		OR has_supervisor_assignment = false;

	IF invalid_count > 0 THEN
		RAISE EXCEPTION 'Backfill aborted: one or more job_id -> worker_user_id mappings failed validation.';
	END IF;
END $$;

WITH input(job_id, worker_user_id) AS (
	VALUES
		('00000000-0000-0000-0000-000000000000'::uuid, '00000000-0000-0000-0000-000000000000'::uuid)
)
UPDATE scoring.scoring_jobs sj
SET
	submitted_by_user_id = input.worker_user_id,
	last_modified = timezone('utc', now()),
	last_modified_by = 'manual-scoring-submitter-backfill'
FROM input
WHERE sj.id = input.job_id
	AND sj.is_deleted = false;

WITH input(job_id, worker_user_id) AS (
	VALUES
		('00000000-0000-0000-0000-000000000000'::uuid, '00000000-0000-0000-0000-000000000000'::uuid)
)
SELECT
	sj.id AS job_id,
	sj.request_id,
	sj.submitted_by_user_id,
	w.id AS worker_id,
	w.full_name AS worker_name
FROM input
JOIN scoring.scoring_jobs sj
	ON sj.id = input.job_id
JOIN workforce.workers w
	ON w.user_id = sj.submitted_by_user_id
	AND w.is_deleted = false
ORDER BY sj.created DESC;

COMMIT;
