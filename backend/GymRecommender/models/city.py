from django.db import models
from .country import Country
import uuid

class City(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    nelatitude = models.FloatField()
    nelongitude = models.FloatField()
    swlatitude = models.FloatField()
    swlongitude = models.FloatField()
    name = models.CharField(max_length=100)
    country = models.ForeignKey(Country, models.DO_NOTHING)

    class Meta:
        managed = False
        db_table = 'city'