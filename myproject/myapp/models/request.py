from django.db import models

class Request(models.Model):
    id = models.UUIDField(primary_key=True)
    requested_at = models.DateTimeField()
    origin_latitude = models.FloatField()
    origin_longitude = models.FloatField()
    departure_time = models.TimeField(blank=True, null=True)
    weekday = models.IntegerField(blank=True, null=True)
    arrival_time = models.TimeField(blank=True, null=True)
    time_priority = models.IntegerField()
    tcost_priority = models.IntegerField()
    price_priority = models.IntegerField()
    rating_priority = models.IntegerField()
    congestion_rating_priority = models.IntegerField()
    name = models.CharField(max_length=50, blank=True, null=True)
    user = models.ForeignKey(Account, models.DO_NOTHING)

    class Meta:
        managed = False
        db_table = 'request'
        unique_together = (('user', 'name'), ('user', 'requested_at'),)

