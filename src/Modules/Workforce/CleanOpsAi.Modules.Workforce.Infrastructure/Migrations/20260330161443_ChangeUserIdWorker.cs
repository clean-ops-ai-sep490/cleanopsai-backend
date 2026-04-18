using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserIdWorker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- 1. Add column mới (uuid)
                ALTER TABLE workforce.workers 
                ADD COLUMN user_id_uuid uuid;

                -- 2. Clean data (trim space)
                UPDATE workforce.workers
                SET user_id = TRIM(user_id);

                -- 3. Convert dữ liệu (chỉ convert thằng hợp lệ)
                UPDATE workforce.workers
                SET user_id_uuid = user_id::uuid
                WHERE user_id IS NOT NULL
                  AND user_id ~* '^[0-9a-fA-F-]{36}$';

                -- 4. Drop column cũ
                ALTER TABLE workforce.workers 
                DROP COLUMN user_id;

                -- 5. Rename lại
                ALTER TABLE workforce.workers 
                RENAME COLUMN user_id_uuid TO user_id;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- rollback về string

                ALTER TABLE workforce.workers 
                ADD COLUMN user_id_text text;

                UPDATE workforce.workers
                SET user_id_text = user_id::text;

                ALTER TABLE workforce.workers 
                DROP COLUMN user_id;

                ALTER TABLE workforce.workers 
                RENAME COLUMN user_id_text TO user_id;
            ");
        }
    }
}
