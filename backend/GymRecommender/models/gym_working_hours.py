from django.db import models
from .gym import Gym
from .working_hours import WorkingHours
import uuid

class GymWorkingHours(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    weekday = models.IntegerField()
    changed_at = models.DateTimeField(blank=True, null=True)
    gym = models.ForeignKey(Gym, models.DO_NOTHING)
    working_hours = models.ForeignKey(WorkingHours, models.DO_NOTHING)

    class Meta:
        managed = False
        db_table = 'gym_working_hours'
        unique_together = (('weekday', 'gym', 'working_hours'),)