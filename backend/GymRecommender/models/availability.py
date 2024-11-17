from django.db import models
from .account import Account
from .gym import Gym
from django.utils.timezone import now
import uuid

class Availability(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    created_at = models.DateTimeField(default=now)
    start_time = models.DateTimeField()
    end_time = models.DateTimeField(blank=True, null=True)
    changed_at = models.DateTimeField(blank=True, null=True)
    gym = models.ForeignKey(Gym, models.DO_NOTHING)
    marked_by = models.ForeignKey(Account, models.DO_NOTHING, db_column='marked_by')

    class Meta:
        managed = False
        db_table = 'availability'
        unique_together = (('gym', 'marked_by'),)