from django.db import models
from .account import Account
import uuid

class RequestPause(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    user = models.OneToOneField(Account, models.DO_NOTHING, blank=True, null=True)
    ip = models.BinaryField(unique=True, blank=True, null=True)
    started_at = models.DateTimeField()

    class Meta:
        managed = False
        db_table = 'request_pause'