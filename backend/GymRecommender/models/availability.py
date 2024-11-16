from django.db import models
from .account import Account
from .gym import Gym

class Availability(models.Model):
    id = models.UUIDField(primary_key=True)
    created_at = models.DateTimeField()
    start_time = models.DateTimeField()
    end_time = models.DateTimeField(blank=True, null=True)
    changed_at = models.DateTimeField(blank=True, null=True)
    gym = models.ForeignKey(Gym, models.DO_NOTHING)
    marked_by = models.ForeignKey(Account, models.DO_NOTHING, db_column='marked_by')

    class Meta:
        managed = False
        db_table = 'availability'
        unique_together = (('gym', 'marked_by'),)