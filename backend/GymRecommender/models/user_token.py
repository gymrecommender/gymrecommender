from django.db import models
from .account import Account
import uuid
from django.utils.timezone import now

class UserToken(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    user = models.OneToOneField(Account, models.DO_NOTHING)
    outer_token = models.TextField(unique=True)
    created_at = models.DateTimeField(default=now)
    updated_at = models.DateTimeField(blank=True, null=True)
    expires_at = models.DateTimeField(blank=True, null=True)

    class Meta:
        managed = False
        db_table = 'user_token'