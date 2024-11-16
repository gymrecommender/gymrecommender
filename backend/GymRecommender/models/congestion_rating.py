from django.db import models
from .gym import Gym
from .account import Account
import uuid

class CongestionRating(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    created_at = models.DateTimeField()
    changed_at = models.DateTimeField()
    visit_time = models.TimeField()
    weekday = models.IntegerField()
    avg_waiting_time = models.IntegerField()
    crowdedness = models.IntegerField()
    gym = models.ForeignKey(Gym, models.DO_NOTHING)
    user = models.ForeignKey(Account, models.DO_NOTHING)

    class Meta:
        managed = False
        db_table = 'congestion_rating'
        unique_together = (('gym', 'user'),)