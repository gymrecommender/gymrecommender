from django.db import models
from .account import Account

class RequestPause(models.Model):
    id = models.UUIDField(primary_key=True)
    user = models.OneToOneField(Account, models.DO_NOTHING, blank=True, null=True)
    ip = models.BinaryField(unique=True, blank=True, null=True)
    started_at = models.DateTimeField()

    class Meta:
        managed = False
        db_table = 'request_pause'