from django.db import models

class Provider(models.TextChoices):
    LOCAL = 'local'
    GOOGLE = 'google'