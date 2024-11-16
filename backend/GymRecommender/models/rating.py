from django.db import models
from .account import Account
from .gym import Gym

class Rating(models.Model):
    id = models.UUIDField(primary_key=True)
    created_at = models.DateTimeField()
    changed_at = models.DateTimeField()
    rating = models.IntegerField()
    user = models.ForeignKey(Account, models.DO_NOTHING)
    gym = models.ForeignKey(Gym, models.DO_NOTHING)

    class Meta:
        managed = False
        db_table = 'rating'
        unique_together = (('user', 'gym'),)