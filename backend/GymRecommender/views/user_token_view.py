from django_filters import rest_framework as filters
from rest_framework.views import APIView
from django.http import Http404
from rest_framework.response import Response
from rest_framework import status
from ..models.user_token import UserToken
from ..serializers import UserTokenSerializer

class UserTokenFilter(filters.FilterSet):
    user = filters.UUIDFilter(field_name='user', lookup_expr='iexact')
    created_at = filters.DateTimeFilter(field_name="created_at", lookup_expr='lte')
    expires_at = filters.DateTimeFilter(field_name="expires_at", lookup_expr='lte')

    class Meta:
        model = UserToken
        fields = ['user', 'created_at', 'expires_at']

class UserTokenView(APIView):
    def get_object(self, pk):
        try:
            return UserToken.objects.get(pk=pk)
        except UserToken.DoesNotExist:
            raise Http404

    def get(self, request, pk=None, format=None):
        if pk:
            return Response(UserTokenSerializer(self.get_object(pk), status=status.HTTP_200_OK))
        else:
            filterset = UserTokenFilter(request.GET, queryset=UserToken.objects.all())
            if not filterset.is_valid():
                return Response(filterset.errors, status=status.HTTP_400_BAD_REQUEST)

            serializer = UserTokenSerializer(filterset.qs, many=True)
            return Response(serializer.data, status=status.HTTP_200_OK)

    def post(self, request, format=None):
        serializer = UserTokenSerializer(data=request.data)
        if serializer.is_valid():
            serializer.save()
            return Response(serializer.data, status=status.HTTP_201_CREATED)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

    def patch(self, request, pk, format=None):
        serializer = UserTokenSerializer(self.get_object(pk), data=request.data, partial=True)
        if serializer.is_valid():
            serializer.save()
            return Response(serializer.data)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

    def delete(self, request, pk, format=None):
        self.get_object(pk).delete()
        return Response(status=status.HTTP_204_NO_CONTENT)