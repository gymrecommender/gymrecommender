from django.db import models
from django.utils.timezone import now
from django.db.models import CheckConstraint, Q, F
import uuid

from ..enums.provider import Provider
from ..enums.account_type import AccountType

class Account(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    username = models.CharField(unique=True, max_length=40)
    outer_uid = models.CharField(unique=True, max_length=128)
    email = models.TextField(unique=True)
    is_email_verified = models.BooleanField(default=False)
    first_name = models.CharField(max_length=60)
    last_name = models.CharField(max_length=60)
    created_at = models.DateTimeField(default=now)
    last_sign_in = models.DateTimeField(blank=True, null=True)
    provider = models.TextField(choices=Provider)
    password_hash = models.CharField(max_length=60)
    type = models.TextField(choices=AccountType)
    created_by = models.ForeignKey('self', models.DO_NOTHING, db_column='created_by', blank=True, null=True)

    class Meta:
        managed = False
        db_table = 'account'
        constraints = [
            CheckConstraint(
                check=Q(username__gt=5),
                name="account_username_min_length"
            ),
            CheckConstraint(
                check=Q(created_at__lte=now()),
                name="account_created_at_is_not_in_the_future"
            ),
            CheckConstraint(
                check=Q(last_sign_in__gt=F('created_at')),
                name="account_last_sign_in_is_after_created_at"
            )
        ]