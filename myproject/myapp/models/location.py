from django.db import models


class City(models.Model):
    id = models.UUIDField(primary_key=True)
    nelatitude = models.FloatField()
    nelongitude = models.FloatField()
    swlatitude = models.FloatField()
    swlongitude = models.FloatField()
    name = models.CharField(max_length=100)
    country = models.ForeignKey('Country', models.DO_NOTHING)

    class Meta:
        managed = False
        db_table = 'city'


class Country(models.Model):
    id = models.UUIDField(primary_key=True)
    name = models.CharField(unique=True, max_length=56)

    class Meta:
        managed = False
        db_table = 'country'
