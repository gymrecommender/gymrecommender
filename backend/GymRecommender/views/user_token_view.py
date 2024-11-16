from rest_framework.views import APIView
from django.http import Http404
from rest_framework.response import Response
from rest_framework import status
from ..models.user_token import UserToken
from ..serializers import UserTokenSerializer

class UserTokenView(APIView):
    def get_object(self, pk):
        try:
            return UserToken.objects.get(pk=pk)
        except UserToken.DoesNotExist:
            raise Http404

    def get(self, request, pk=None, format=None):
        serializer = UserTokenSerializer(UserToken.objects.all(), many=True) if not pk else UserTokenSerializer(
            self.get_object(pk))

        return Response(serializer.data)

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