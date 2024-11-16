from rest_framework.views import APIView
from django.http import Http404
from rest_framework.response import Response
from rest_framework import status
from ..models.gym_working_hours import GymWorkingHours
from ..serializers import GymWorkingHoursSerializer

class GymWorkingHoursView(APIView):
    def get_object(self, pk):
        try:
            return GymWorkingHours.objects.get(pk=pk)
        except GymWorkingHours.DoesNotExist:
            raise Http404

    def get(self, request, pk=None, format=None):
        serializer = GymWorkingHoursSerializer(GymWorkingHours.objects.all(), many=True) if not pk else GymWorkingHoursSerializer(
            self.get_object(pk))

        return Response(serializer.data)

    def post(self, request, format=None):
        serializer = GymWorkingHoursSerializer(data=request.data)
        if serializer.is_valid():
            serializer.save()
            return Response(serializer.data, status=status.HTTP_201_CREATED)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

    def patch(self, request, pk, format=None):
        serializer = GymWorkingHoursSerializer(self.get_object(pk), data=request.data, partial=True)
        if serializer.is_valid():
            serializer.save()
            return Response(serializer.data)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

    def delete(self, request, pk, format=None):
        self.get_object(pk).delete()
        return Response(status=status.HTTP_204_NO_CONTENT)