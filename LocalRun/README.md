# Local Development Setup

If you want to run this project locally as containers, follow the steps below:

1. Navigate to the **LocalRun** folder:

   ```bash
   cd LocalRun
   ```

2. Start the dependencies using Docker Compose:
   ```bash
   docker-compose up -d
   ```

---

## Setting Up the Initial Admin User

Once the containers are up and running, you can connect to the Postgres database using your preferred SQL client.

**Database connection details**:

- Database name: mini_url
- Username: postgres
- Password: admin

After connecting, run the following **SQL** command to create the initial admin user account:

```bash
INSERT INTO public."Users" ("Id", "Email", "Username", "Password", "Role") VALUES
('0199b46b-2762-7c8d-aa1e-91363a2c59b9'::uuid, 'admin@gmail.com', 'admin',
'$2a$11$jxARUCXQeHjG0D3V1K./A.sf8Vh4jZxccp0hL.mN1HmlGkTo98KrW', 'Admin');
```

### Application Admin User credentials (You can use this admin account to APPROVE or REJECT user urls)

- Username: admin
- Password: admin
