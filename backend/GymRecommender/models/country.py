from django.db import models

class Country(models.Model):
    id = models.UUIDField(primary_key=True)
    name = models.CharField(unique=True, max_length=56)

    class Meta:
        managed = False
        db_table = 'country'