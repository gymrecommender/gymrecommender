from django.db import models
from .gym import Gym
from .request import Request
from .currency import Currency
from ..enums.rec_type import RecType

class Recommendation(models.Model):
    id = models.UUIDField(primary_key=True)
    tcost = models.DecimalField(max_digits=4, decimal_places=2)
    time = models.TimeField()
    time_score = models.DecimalField(max_digits=4, decimal_places=2)
    tcost_score = models.DecimalField(max_digits=4, decimal_places=2)
    congestion_score = models.DecimalField(max_digits=4, decimal_places=2, blank=True, null=True)
    rating_score = models.DecimalField(max_digits=4, decimal_places=2, blank=True, null=True)
    total_score = models.DecimalField(max_digits=4, decimal_places=2)
    type = models.TextField(choices=RecType)
    gym = models.ForeignKey(Gym, models.DO_NOTHING)
    request = models.ForeignKey(Request, models.DO_NOTHING)
    currency = models.ForeignKey(Currency, models.DO_NOTHING)

    class Meta:
        managed = False
        db_table = 'recommendation'
        unique_together = (('gym', 'request'),)