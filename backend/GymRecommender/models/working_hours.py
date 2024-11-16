from django.db import models

class WorkingHours(models.Model):
    id = models.UUIDField(primary_key=True)
    open_from = models.TimeField()
    open_until = models.TimeField()

    class Meta:
        managed = False
        db_table = 'working_hours'
        unique_together = (('open_from', 'open_until'),)