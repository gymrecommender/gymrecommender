from django.db import models

class RecType(models.TextChoices):
    MAIN="main"
    ALTERNATIVE="alternative"