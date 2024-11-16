from django.db import models

class NotificationType(models.TextChoices):
    MESSAGE="message"
    ALERT="alert"
    REMINDER="reminder"