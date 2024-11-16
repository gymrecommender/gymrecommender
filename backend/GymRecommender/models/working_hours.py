from django.db import models
import uuid

class WorkingHours(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    open_from = models.TimeField()
    open_until = models.TimeField()

    class Meta:
        managed = False
        db_table = 'working_hours'
        unique_together = (('open_from', 'open_until'),)