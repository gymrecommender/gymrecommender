from django.db import models
from .account import Account
from ..enums.notification_type import NotificationType
import uuid

class Notification(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    type = models.TextField(choices=NotificationType)
    message = models.TextField()
    created_at = models.DateTimeField()
    read_at = models.DateTimeField(blank=True, null=True)
    user = models.ForeignKey(Account, models.DO_NOTHING)

    class Meta:
        managed = False
        db_table = 'notification'