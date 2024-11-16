from django.db import models
from .account import Account
from .gym import Gym
from ..enums.own_decision import OwnDecision
import uuid

class Ownership(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    requested_at = models.DateTimeField()
    responded_at = models.DateTimeField(blank=True, null=True)
    decision = models.TextField(choices=OwnDecision)
    message = models.TextField(blank=True, null=True)
    responded_by = models.ForeignKey(Account, models.DO_NOTHING, db_column='responded_by', blank=True, null=True)
    requested_by = models.ForeignKey(Account, models.DO_NOTHING, db_column='requested_by', related_name='ownership_requested_by_set')
    gym = models.ForeignKey(Gym, models.DO_NOTHING)

    class Meta:
        managed = False
        db_table = 'ownership'
        unique_together = (('gym', 'requested_by', 'decision'),)