from django.db import models

class OwnDecision(models.TextChoices):
    REJECTED="rejected"
    APPROVED="approved"