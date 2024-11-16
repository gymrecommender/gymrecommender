from django.db import models
from .account import Account
from .gym import Gym
import uuid

class Bookmark(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    created_at = models.DateTimeField()
    user = models.ForeignKey(Gym, models.DO_NOTHING)
    gym = models.ForeignKey(Account, models.DO_NOTHING)

    class Meta:
        managed = False
        db_table = 'bookmark'
        unique_together = (('user', 'gym'),)