from django.db import models
from .account import Account

class UserToken(models.Model):
    id = models.UUIDField(primary_key=True)
    user = models.OneToOneField(Account, models.DO_NOTHING)
    outer_token = models.TextField(unique=True)
    created_at = models.DateTimeField()
    updated_at = models.DateTimeField()
    expires_at = models.DateTimeField()

    class Meta:
        managed = False
        db_table = 'user_token'