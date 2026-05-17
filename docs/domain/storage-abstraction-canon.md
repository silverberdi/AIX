# Storage Abstraction Canon

AIX manages storage capabilities, not provider-specific implementations.

Storage capabilities:
- upload
- download
- versioning
- immutability
- tiering
- legal hold
- restore
- signed URLs

Storage belongs to the tenant.

Each tenant has one active storage strategy in the MVP.

AIX owns storage organization.

Before COMPLETE:
- files may be replaced
- files may be deleted physically
- upload may fail/retry

After COMPLETE:
- files become governed
- files become immutable
- retention/audit/policies apply

Some DocumentTypes are versionable.
Versionable documents never overwrite physical evidence.

Default access strategy:
- signed URLs

Sensitive documents:
- backend streaming/proxy
