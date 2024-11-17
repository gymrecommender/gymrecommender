from django_filters import rest_framework as filters
from rest_framework.views import APIView
from django.http import Http404
from rest_framework.response import Response
from rest_framework import status
from GymRecommender.models.account import Account
from GymRecommender.serializers import AccountSerializer
from bcrypt import hashpw, gensalt

class AccountFilter(filters.FilterSet):
    username = filters.CharFilter(field_name='username', lookup_expr='iexact')
    email = filters.CharFilter(field_name="email", lookup_expr='iexact')
    is_email_verified = filters.BooleanFilter(field_name="is_email_verified", lookup_expr='iexact')
    type = filters.CharFilter(field_name="type", lookup_expr="iexact")

    class Meta:
        model = Account
        fields = ['username', 'email', 'is_email_verified', 'type']


class AccountView(APIView):
    def get_object(self, pk):
        try:
            return Account.objects.get(pk=pk)
        except Account.DoesNotExist:
            raise Http404

    def get(self, request, pk=None, format=None):
        if pk:
            return Response(AccountSerializer(self.get_object(pk), status=status.HTTP_200_OK))
        else:
            filterset = AccountFilter(request.GET, queryset=Account.objects.all())
            if not filterset.is_valid():
                return Response(filterset.errors, status=status.HTTP_400_BAD_REQUEST)

            serializer = AccountSerializer(filterset.qs, many=True)
            return Response(serializer.data, status=status.HTTP_200_OK)

    def post(self, request, format=None):
        request.data["password_hash"] = hashpw(request.data['password'].encode('utf-8'), gensalt()).decode("utf-8")
        request.data.pop("password", None)

        serializer = AccountSerializer(data=request.data)
        if serializer.is_valid():
            serializer.save()
            return Response(serializer.data, status=status.HTTP_201_CREATED)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

    def patch(self, request, pk, format=None):
        serializer = AccountSerializer(self.get_object(pk), data=request.data, partial=True)
        if serializer.is_valid():
            serializer.save()
            return Response(serializer.data)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

    def delete(self, request, pk, format=None):
        self.get_object(pk).delete()
        return Response(status=status.HTTP_204_NO_CONTENT)