from django.db import models
from .account import Account
import uuid

class Request(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    requested_at = models.DateTimeField()
    origin_latitude = models.FloatField()
    origin_longitude = models.FloatField()
    time_priority = models.IntegerField()
    total_cost_priority = models.IntegerField()
    min_congestion_rating = models.DecimalField(max_digits=4, decimal_places=2)
    min_rating = models.DecimalField(max_digits=4, decimal_places=2)
    min_membership_price = models.IntegerField()
    name = models.CharField(max_length=50, blank=True, null=True)
    user = models.ForeignKey(Account, models.DO_NOTHING)

    class Meta:
        managed = False
        db_table = 'request'
        unique_together = (('user', 'name'), ('user', 'requested_at'),)