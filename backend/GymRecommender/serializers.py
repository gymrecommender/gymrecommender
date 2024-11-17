from rest_framework import serializers
from torch.optim.optimizer import required

from .models import *

class AccountSerializer(serializers.ModelSerializer):
    created_by_data = serializers.SerializerMethodField()
    # created_by = serializers.UUIDField(required=False)

    class Meta:
        model = Account
        fields = '__all__'

    def get_created_by_data(self, obj):
        return AccountSerializer(obj.created_by).data if obj.created_by is not None else None

class AvailabilitySerializer(serializers.ModelSerializer):
    gym_data = serializers.SerializerMethodField()
    marked_by_data = serializers.SerializerMethodField()
    end_time = serializers.DateTimeField(required=False)
    changed_at = serializers.DateTimeField(required=False)

    class Meta:
        model = Availability
        fields = '__all__'

    def get_gym_data(self, obj):
        return GymSerializer(obj.gym).data

    def get_marked_by_data(self, obj):
        return AccountSerializer(obj.marked_by).data

class BookmarkSerializer(serializers.ModelSerializer):
    user_data = serializers.SerializerMethodField()
    gym_data = serializers.SerializerMethodField()

    class Meta:
        model = Bookmark
        fields = '__all__'

    def get_gym_data(self, obj):
        return GymSerializer(obj.gym).data

    def get_user_data(self, obj):
        return AccountSerializer(obj.user).data

class CitySerializer(serializers.ModelSerializer):
    country_data = serializers.SerializerMethodField()

    class Meta:
        model = City
        fields = '__all__'

    def get_country_data(self, obj):
        return CountrySerializer(obj.country).data

class CongestionRatingSerializer(serializers.ModelSerializer):
    gym_data = serializers.SerializerMethodField()
    user_data = serializers.SerializerMethodField()
    changed_at = serializers.DateTimeField(required=False)

    class Meta:
        model = CongestionRating
        fields = '__all__'

    def get_gym_data(self, obj):
        return GymSerializer(obj.gym).data

    def get_user_data(self, obj):
        return AccountSerializer(obj.user).data

class CountrySerializer(serializers.ModelSerializer):
    class Meta:
        model = Country
        fields = '__all__'

class CurrencySerializer(serializers.ModelSerializer):
    class Meta:
        model = Currency
        fields = '__all__'

class GymSerializer(serializers.ModelSerializer):
    owned_by_data = serializers.SerializerMethodField()
    city_data = serializers.SerializerMethodField()
    currency_data = serializers.SerializerMethodField()

    class Meta:
        model = Currency
        fields = '__all__'

    def get_owned_by_data(self, obj):
        return AccountSerializer(obj.owned_by).data if obj.owned_by is not None else None

    def get_city_data(self, obj):
        return CitySerializer(obj.city).data

    def get_currency_data(self, obj):
        return CurrencySerializer(obj.currency).data


class GymWorkingHoursSerializer(serializers.ModelSerializer):
    gym_data = serializers.SerializerMethodField()
    working_hours_data = serializers.SerializerMethodField()

    class Meta:
        model = GymWorkingHours
        fields = '__all__'

    def get_gym_data(self, obj):
        return GymSerializer(obj.gym).data

    def get_working_hours_data(self, obj):
        return WorkingHoursSerializer(obj.working_hours).data

class NotificationSerializer(serializers.ModelSerializer):
    user_data = serializers.SerializerMethodField()

    class Meta:
        model = Notification
        fields = '__all__'

    def get_user_data(self, obj):
        return AccountSerializer(obj.user).data

class OwnershipSerializer(serializers.ModelSerializer):
    responded_by_data = serializers.SerializerMethodField()
    requested_by_data = serializers.SerializerMethodField()
    gym_data = serializers.SerializerMethodField()

    class Meta:
        model = Ownership
        fields = '__all__'

    def get_gym_data(self, obj):
        return GymSerializer(obj.gym).data

    def get_responded_by_data(self, obj):
        return AccountSerializer(obj.responded_by).data if obj.responded_by is not None else None

    def get_requested_by_data(self, obj):
        return AccountSerializer(obj.requested_by).data

class RatingSerializer(serializers.ModelSerializer):
    gym_data = serializers.SerializerMethodField()
    user_data = serializers.SerializerMethodField()

    class Meta:
        model = Rating
        fields = '__all__'

    def get_user_data(self, obj):
        return AccountSerializer(obj.user).data

    def get_gym_data(self, obj):
        return GymSerializer(obj.gym).data


class RecommendationSerializer(serializers.ModelSerializer):
    gym_data = serializers.SerializerMethodField()
    request_data = serializers.SerializerMethodField()
    currency_data = serializers.SerializerMethodField()

    class Meta:
        model = Recommendation
        fields = '__all__'

    def get_gym_data(self, obj):
        return GymSerializer(obj.gym).data

    def get_request_data(self, obj):
        return RequestSerializer(obj.request).data

    def get_currency_data(self, obj):
        return CurrencySerializer(obj.currency).data



class RequestSerializer(serializers.ModelSerializer):
    user_data = serializers.SerializerMethodField()

    class Meta:
        model = Request
        fields = '__all__'

    def get_user_data(self, obj):
        return AccountSerializer(obj.user).data

class RequestPauseSerializer(serializers.ModelSerializer):
    user_data = serializers.SerializerMethodField()

    class Meta:
        model = RequestPause
        fields = '__all__'

    def get_user_data(self, obj):
        return AccountSerializer(obj.user).data if obj.user is not None else None

class RequestPeriodSerializer(serializers.ModelSerializer):
    request = serializers.SerializerMethodField()

    class Meta:
        model = RequestPeriod
        fields = '__all__'

class UserTokenSerializer(serializers.ModelSerializer):
    user_data = serializers.SerializerMethodField()
    updated_at = serializers.DateTimeField(required=False)
    expires_at = serializers.DateTimeField(required=False)

    class Meta:
        model = UserToken
        fields = '__all__'

    def get_user_data(self, obj):
        return AccountSerializer(obj.user).data

class WorkingHoursSerializer(serializers.ModelSerializer):
    class Meta:
        model = WorkingHours
        fields = '__all__'