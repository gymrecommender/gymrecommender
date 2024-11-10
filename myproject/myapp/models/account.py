from django.db import models


class Account(models.Model):
    id = models.UUIDField(primary_key=True)
    username = models.CharField(unique=True, max_length=40)
    email = models.CharField(unique=True, max_length=60)
    first_name = models.CharField(max_length=60)
    last_name = models.CharField(max_length=60)
    created_at = models.DateTimeField()
    provider = models.TextField()  # This field type is a guess.
    password_hash = models.CharField(max_length=60)
    type = models.TextField()  # This field type is a guess.
    created_by = models.ForeignKey('self', models.DO_NOTHING, db_column='created_by', blank=True, null=True)

    class Meta:
        managed = False
        db_table = 'account'





class Notification(models.Model):
    id = models.UUIDField(primary_key=True)
    type = models.TextField()  # This field type is a guess.
    message = models.TextField()
    created_at = models.DateTimeField()
    read_at = models.DateField(blank=True, null=True)
    user = models.ForeignKey(Account, models.DO_NOTHING)

    class Meta:
        managed = False
        db_table = 'notification'


class Ownership(models.Model):
    id = models.UUIDField(primary_key=True)
    requested_at = models.DateTimeField()
    responded_at = models.DateTimeField(blank=True, null=True)
    status = models.TextField()  # This field type is a guess.
    message = models.TextField(blank=True, null=True)
    responded_by = models.ForeignKey(Account, models.DO_NOTHING, db_column='responded_by', blank=True, null=True)
    requested_by = models.ForeignKey(Account, models.DO_NOTHING, db_column='requested_by', related_name='ownership_requested_by_set')
    gym_id = models.UUIDField()

    class Meta:
        managed = False
        db_table = 'ownership'
        unique_together = (('gym_id', 'requested_by', 'status'),)
