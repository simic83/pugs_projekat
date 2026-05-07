# Trip Planning Migrations

Trip planning migrations:

1. `001_create_trip_planning_schema.sql` - trip plans, destinations and activities.
2. `002_create_checklist_items.sql` - checklist / packing lista.
3. `003_create_notes.sql` - notes / beleske za plan putovanja.
4. `004_create_reminders.sql` - podsjetnici za plan putovanja.
5. `005_add_trip_plan_owner_cascade.sql` - cascade delete veza izmedju korisnika i njegovih planova.
6. `006_add_required_date_checks.sql` - CHECK constrainti koji odbijaju default datume plana, destinacija i aktivnosti.
