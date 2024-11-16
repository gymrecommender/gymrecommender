from rest_framework.views import APIView
from django.http import Http404
from rest_framework.response import Response
from rest_framework import status
from ..models.ownership import Ownership
from ..serializers import OwnershipSerializer

class OwnershipView(APIView):
    def get_object(self, pk):
        try:
            return Ownership.objects.get(pk=pk)
        except Ownership.DoesNotExist:
            raise Http404

    def get(self, request, pk=None, format=None):
        serializer = OwnershipSerializer(Ownership.objects.all(), many=True) if not pk else OwnershipSerializer(
            self.get_object(pk))

        return Response(serializer.data)

    def post(self, request, format=None):
        serializer = OwnershipSerializer(data=request.data)
        if serializer.is_valid():
            serializer.save()
            return Response(serializer.data, status=status.HTTP_201_CREATED)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

    def patch(self, request, pk, format=None):
        serializer = OwnershipSerializer(self.get_object(pk), data=request.data, partial=True)
        if serializer.is_valid():
            serializer.save()
            return Response(serializer.data)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

    def delete(self, request, pk, format=None):
        self.get_object(pk).delete()
        return Response(status=status.HTTP_204_NO_CONTENT)