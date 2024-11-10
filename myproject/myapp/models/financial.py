from django.db import models

class Currency(models.Model):
    id = models.UUIDField(primary_key=True)
    name = models.CharField(unique=True, max_length=10)
    code = models.CharField(unique=True, max_length=3)

    class Meta:
        managed = False
        db_table = 'currency'
