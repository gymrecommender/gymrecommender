from rest_framework import serializers

from .models import *

class RecursiveField(serializers.ModelSerializer):
    def to_representation(self, value):
        serializer = self.parent.__class__(value, context=self.context)
        return serializer.data

class AccountSerializer(serializers.ModelSerializer):
    created_by = RecursiveField("created_by")

    class Meta:
        model = Account
        fields = '__all__'

class AvailabilitySerializer(serializers.ModelSerializer):
    gym = RecursiveField("gym")
    marked_by = RecursiveField("marked_by")

    class Meta:
        model = Availability
        fields = '__all__'

class BookmarkSerializer(serializers.ModelSerializer):
    user = RecursiveField("user")
    gym = RecursiveField("gym")

    class Meta:
        model = Bookmark
        fields = '__all__'

class CitySerializer(serializers.ModelSerializer):
    country = RecursiveField("country")

    class Meta:
        model = City
        fields = '__all__'

class CongestionRatingSerializer(serializers.ModelSerializer):
    gym = RecursiveField("gym")
    user = RecursiveField("user")

    class Meta:
        model = CongestionRating
        fields = '__all__'

class CountrySerializer(serializers.ModelSerializer):
    class Meta:
        model = Country
        fields = '__all__'

class CurrencySerializer(serializers.ModelSerializer):
    class Meta:
        model = Currency
        fields = '__all__'

class GymSerializer(serializers.ModelSerializer):
    owned_by = RecursiveField("owned_by")
    city = RecursiveField("city")
    currency = RecursiveField("currency")

    class Meta:
        model = Currency
        fields = '__all__'

class GymWorkingHoursSerializer(serializers.ModelSerializer):
    gym = RecursiveField("gym")
    working_hours = RecursiveField("working_hours")

    class Meta:
        model = GymWorkingHours
        fields = '__all__'

class NotificationSerializer(serializers.ModelSerializer):
    user = RecursiveField("user")

    class Meta:
        model = Notification
        fields = '__all__'

class OwnershipSerializer(serializers.ModelSerializer):
    responded_by = RecursiveField("responded_by")
    requested_by = RecursiveField("requested_by")
    gym = RecursiveField("gym")

    class Meta:
        model = Ownership
        fields = '__all__'

class RatingSerializer(serializers.ModelSerializer):
    user = RecursiveField("user")
    gym = RecursiveField("gym")

    class Meta:
        model = Rating
        fields = '__all__'

class RecommendationSerializer(serializers.ModelSerializer):
    gym = RecursiveField("gym")
    request = RecursiveField("request")
    currency = RecursiveField("currency")

    class Meta:
        model = Recommendation
        fields = '__all__'

class RequestSerializer(serializers.ModelSerializer):
    user = RecursiveField("user")

    class Meta:
        model = Request
        fields = '__all__'

class RequestPauseSerializer(serializers.ModelSerializer):
    user = RecursiveField("user")

    class Meta:
        model = RequestPause
        fields = '__all__'

class RequestPeriodSerializer(serializers.ModelSerializer):
    request = RecursiveField("request")

    class Meta:
        model = RequestPeriod
        fields = '__all__'

class UserTokenSerializer(serializers.ModelSerializer):
    user = RecursiveField("user")

    class Meta:
        model = UserToken
        fields = '__all__'

class WorkingHoursSerializer(serializers.ModelSerializer):
    class Meta:
        model = WorkingHours
        fields = '__all__'