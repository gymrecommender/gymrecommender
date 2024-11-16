from rest_framework.views import APIView
from django.http import Http404
from rest_framework.response import Response
from rest_framework import status
from ..models.availability import Availability
from ..serializers import AvailabilitySerializer

class AvailabilityView(APIView):
    def get_object(self, pk):
        try:
            return Availability.objects.get(pk=pk)
        except Availability.DoesNotExist:
            raise Http404

    def get(self, request, pk, format=None):
        return Response(AvailabilitySerializer(self.get_object(pk)).data)

    def post(self, request, format=None):
        serializer = AvailabilitySerializer(data=request.data)
        if serializer.is_valid():
            serializer.save()
            return Response(serializer.data, status=status.HTTP_201_CREATED)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

    def put(self, request, pk, format=None):
        snippet = self.get_object(pk)
        serializer = AvailabilitySerializer(snippet, data=request.data)
        if serializer.is_valid():
            serializer.save()
            return Response(serializer.data)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

    def delete(self, request, pk, format=None):
        snippet = self.get_object(pk)
        snippet.delete()
        return Response(status=status.HTTP_204_NO_CONTENT)