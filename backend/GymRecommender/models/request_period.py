from django.db import models
from .request import Request

class RequestPeriod(models.Model):
    id = models.UUIDField(primary_key=True)
    request = models.ForeignKey(Request, models.DO_NOTHING)
    weekday = models.IntegerField()
    arrival_time = models.TimeField(blank=True, null=True)
    departure_time = models.TimeField(blank=True, null=True)

    class Meta:
        managed = False
        db_table = 'request_period'
        unique_together = (('request', 'weekday'),)