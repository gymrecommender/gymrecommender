from django.db import models
from .currency import Currency
from .account import Account
from .city import City
import uuid

class Gym(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    latitude = models.FloatField()
    longitude = models.FloatField()
    name = models.CharField(max_length=80)
    external_place_id = models.CharField(unique=True, max_length=50)
    external_rating = models.DecimalField(max_digits=4, decimal_places=2)
    external_rating_number = models.IntegerField()
    phone_number = models.CharField(max_length=15, blank=True, null=True)
    address = models.TextField()
    website = models.CharField(max_length=255, blank=True, null=True)
    is_wheelchair_accessible = models.BooleanField()
    monthly_mprice = models.DecimalField(max_digits=5, decimal_places=2, blank=True, null=True)
    yearly_mprice = models.DecimalField(max_digits=5, decimal_places=2, blank=True, null=True)
    six_months_mprice = models.DecimalField(max_digits=5, decimal_places=2, blank=True, null=True)
    created_at = models.DateTimeField()
    price_changed_at = models.DateTimeField(blank=True, null=True)
    changed_at = models.DateTimeField(blank=True, null=True)
    internal_rating = models.DecimalField(max_digits=4, decimal_places=2)
    internal_rating_number = models.IntegerField()
    congestion_rating = models.DecimalField(max_digits=4, decimal_places=2)
    congestion_rating_number = models.IntegerField()
    owned_by = models.ForeignKey(Account, models.DO_NOTHING, db_column='owned_by', blank=True, null=True)
    currency = models.ForeignKey(Currency, models.DO_NOTHING)
    city = models.ForeignKey(City, models.DO_NOTHING)

    class Meta:
        managed = False
        db_table = 'gym'