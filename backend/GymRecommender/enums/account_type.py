from django.db import models

class AccountType(models.TextChoices):
    USER = "user"
    GYM = "gym"
    ADMIN = "admin"