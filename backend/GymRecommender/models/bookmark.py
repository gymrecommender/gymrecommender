from django.db import models
from .account import Account
from .gym import Gym

class Bookmark(models.Model):
    id = models.UUIDField(primary_key=True)
    created_at = models.DateTimeField()
    user = models.ForeignKey(Gym, models.DO_NOTHING)
    gym = models.ForeignKey(Account, models.DO_NOTHING)

    class Meta:
        managed = False
        db_table = 'bookmark'
        unique_together = (('user', 'gym'),)