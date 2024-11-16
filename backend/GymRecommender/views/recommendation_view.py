from rest_framework.views import APIView
from django.http import Http404
from rest_framework.response import Response
from rest_framework import status
from ..models.recommendation import Recommendation
from ..serializers import RecommendationSerializer

class RecommendationView(APIView):
    def get_object(self, pk):
        try:
            return Recommendation.objects.get(pk=pk)
        except Recommendation.DoesNotExist:
            raise Http404

    def get(self, request, pk=None, format=None):
        serializer = RecommendationSerializer(Recommendation.objects.all(), many=True) if not pk else RecommendationSerializer(
            self.get_object(pk))

        return Response(serializer.data)

    def post(self, request, format=None):
        serializer = RecommendationSerializer(data=request.data)
        if serializer.is_valid():
            serializer.save()
            return Response(serializer.data, status=status.HTTP_201_CREATED)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

    def patch(self, request, pk, format=None):
        serializer = RecommendationSerializer(self.get_object(pk), data=request.data, partial=True)
        if serializer.is_valid():
            serializer.save()
            return Response(serializer.data)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

    def delete(self, request, pk, format=None):
        self.get_object(pk).delete()
        return Response(status=status.HTTP_204_NO_CONTENT)